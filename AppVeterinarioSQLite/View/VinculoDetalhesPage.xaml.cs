using AppVeterinarioSQLite.Models;

namespace AppVeterinarioSQLite.Views;

public partial class VinculoDetalhesPage : ContentPage
{
    private List<Cliente> _clientes;
    private List<Animal> _animais;

    public VinculoDetalhesPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarClientes();
        await CarregarAnimais();
    }

    private async Task CarregarClientes()
    {
        try
        {
            _clientes = await App.Db.GetAllClientes();

            pickerCliente.Items.Clear();
            foreach (var cliente in _clientes)
            {
                pickerCliente.Items.Add(cliente.Nome);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível carregar os clientes: {ex.Message}", "OK");
        }
    }

    private async Task CarregarAnimais()
    {
        try
        {
            _animais = await App.Db.GetAllAnimais();

            pickerAnimal.Items.Clear();
            foreach (var animal in _animais)
            {
                pickerAnimal.Items.Add($"{animal.Nome} ({animal.EspecieNome})");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível carregar os animais: {ex.Message}", "OK");
        }
    }

    private async void OnSalvarClicked(object sender, EventArgs e)
    {
        bool isValid = true;

        if (pickerCliente.SelectedIndex < 0)
        {
            lblErroCliente.IsVisible = true;
            isValid = false;
        }
        else
        {
            lblErroCliente.IsVisible = false;
        }

        if (pickerAnimal.SelectedIndex < 0)
        {
            lblErroAnimal.IsVisible = true;
            isValid = false;
        }
        else
        {
            lblErroAnimal.IsVisible = false;
        }

        if (!isValid) return;

        try
        {
            var clienteSelecionado = _clientes[pickerCliente.SelectedIndex];
            var animalSelecionado = _animais[pickerAnimal.SelectedIndex];

            var vinculosExistentes = await App.Db.GetAnimaisByCliente(clienteSelecionado.Id);
            bool vinculoJaExiste = vinculosExistentes.Any(v => v.AnimalId == animalSelecionado.Id);

            if (vinculoJaExiste)
            {
                await DisplayAlert("Aviso",
                    $"O vínculo entre '{clienteSelecionado.Nome}' e '{animalSelecionado.Nome}' já existe!",
                    "OK");
                return;
            }

            var novoVinculo = new AnimalCliente
            {
                ClienteId = clienteSelecionado.Id,
                AnimalId = animalSelecionado.Id,
                ClienteNome = clienteSelecionado.Nome,
                AnimalNome = animalSelecionado.Nome
            };

            await App.Db.InsertAnimalCliente(novoVinculo);
            await DisplayAlert("Sucesso",
                $"Vínculo criado com sucesso!\n'{clienteSelecionado.Nome}' ↔ '{animalSelecionado.Nome}'",
                "OK");

            pickerCliente.SelectedIndex = -1;
            pickerAnimal.SelectedIndex = -1;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível criar o vínculo: {ex.Message}", "OK");
        }
    }

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
