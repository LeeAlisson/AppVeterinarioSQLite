using AppVeterinarioSQLite.Models;

namespace AppVeterinarioSQLite.Views;

[QueryProperty(nameof(EspecieId), "id")]
[QueryProperty(nameof(EspecieObj), "especie")]
public partial class EspecieDetalhesPage : ContentPage
{
    private int _especieId;
    private Especie _especie;
    private bool _isEdicao = false;

    public string EspecieId
    {
        set
        {
            _especieId = int.Parse(value);
            CarregarEspecie(_especieId);
        }
    }

    public Especie EspecieObj
    {
        set
        {
            _especie = value;
            if (_especie != null)
            {
                CarregarEspecieFromObject(_especie);
            }
        }
    }

    public EspecieDetalhesPage()
    {
        InitializeComponent();
    }

    private async void CarregarEspecie(int id)
    {
        try
        {
            _especie = await App.Db.GetEspecieById(id);
            if (_especie != null)
            {
                CarregarEspecieFromObject(_especie);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível carregar a espécie: {ex.Message}", "OK");
        }
    }

    private async void CarregarEspecieFromObject(Especie especie)
    {
        _isEdicao = true;

        lblTitulo.Text = "Editar Espécie";
        containerID.IsVisible = true;
        btnExcluir.IsVisible = true;
        entryID.Text = especie.Id.ToString();
        entryNome.Text = especie.Nome;

        try
        {
            var animais = await App.Db.GetAnimaisByEspecie(especie.Id);

            if (animais.Any())
            {
                frameAnimais.IsVisible = true;
                collectionViewAnimais.ItemsSource = animais;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Aviso", $"Não foi possível carregar os animais relacionados: {ex.Message}", "OK");
        }
    }

    private async void OnSalvarClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(entryNome.Text))
        {
            lblErroNome.IsVisible = true;
            return;
        }

        lblErroNome.IsVisible = false;

        try
        {
            if (_isEdicao)
            {
                _especie.Nome = entryNome.Text;
                await App.Db.Update(_especie);
                await DisplayAlert("Sucesso", "Espécie atualizada com sucesso!", "OK");
            }
            else
            {
                var novaEspecie = new Especie
                {
                    Nome = entryNome.Text
                };

                await App.Db.Insert(novaEspecie);
                await DisplayAlert("Sucesso", "Espécie cadastrada com sucesso!", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível salvar a espécie: {ex.Message}", "OK");
        }
    }

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnExcluirClicked(object sender, EventArgs e)
    {
        if (_especie == null) return;

        try
        {
            bool podeExcluir = await App.Db.PodeExcluirEspecie(_especie.Id);

            if (!podeExcluir)
            {
                bool forcarExclusao = await DisplayAlert("Atenção!",
                    $"A espécie '{_especie.Nome}' possui animais vinculados. Deseja excluir mesmo assim? Isso também excluirá todos os animais desta espécie e seus vínculos.",
                    "Sim, excluir tudo", "Cancelar");

                if (!forcarExclusao) return;

                var animais = await App.Db.GetAnimaisByEspecie(_especie.Id);

                foreach (var animal in animais)
                {
                    await App.Db.DeleteVinculosDoAnimal(animal.Id);
                }

                foreach (var animal in animais)
                {
                    await App.Db.DeleteAnimal(animal.Id);
                }
            }
            else
            {
                bool confirmar = await DisplayAlert("Confirmar exclusão",
                    $"Deseja realmente excluir a espécie '{_especie.Nome}'?",
                    "Sim", "Não");

                if (!confirmar) return;
            }

            await App.Db.Delete(_especie.Id);
            await DisplayAlert("Sucesso", "Espécie excluída com sucesso!", "OK");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível excluir a espécie: {ex.Message}", "OK");
        }
    }
}
