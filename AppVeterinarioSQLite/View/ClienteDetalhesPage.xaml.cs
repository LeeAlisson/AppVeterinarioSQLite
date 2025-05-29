using AppVeterinarioSQLite.Models;
using System.Text.RegularExpressions;

namespace AppVeterinarioSQLite.Views;

[QueryProperty(nameof(ClienteId), "id")]
[QueryProperty(nameof(ClienteObj), "cliente")]
public partial class ClienteDetalhesPage : ContentPage
{
    private int _clienteId;
    private Cliente _cliente;
    private bool _isEdicao = false;

    public string ClienteId
    {
        set
        {
            _clienteId = int.Parse(value);
            CarregarCliente(_clienteId);
        }
    }

    public Cliente ClienteObj
    {
        set
        {
            _cliente = value;
            if (_cliente != null)
            {
                CarregarClienteFromObject(_cliente);
            }
        }
    }

    public ClienteDetalhesPage()
    {
        InitializeComponent();
    }

    private async void CarregarCliente(int id)
    {
        try
        {
            _cliente = await App.Db.GetClienteById(id);
            if (_cliente != null)
            {
                CarregarClienteFromObject(_cliente);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível carregar o cliente: {ex.Message}", "OK");
        }
    }

    private void CarregarClienteFromObject(Cliente cliente)
    {
        _isEdicao = true;

        lblTitulo.Text = "Editar Cliente";
        containerID.IsVisible = true;
        containerDataCadastro.IsVisible = true;
        btnExcluir.IsVisible = true;

        entryID.Text = cliente.Id.ToString();
        entryNome.Text = cliente.Nome;
        entryCPF.Text = cliente.CPF.ToString();
        entryEmail.Text = cliente.Email;
        entryDataCadastro.Text = cliente.DataCadastro.ToString("dd/MM/yyyy");
    }

    private async void OnSalvarClicked(object sender, EventArgs e)
    {
        bool isValid = true;

        if (string.IsNullOrWhiteSpace(entryNome.Text))
        {
            lblErroNome.IsVisible = true;
            isValid = false;
        }
        else
        {
            lblErroNome.IsVisible = false;
        }

        if (string.IsNullOrWhiteSpace(entryCPF.Text) || entryCPF.Text.Length != 11)
        {
            lblErroCPF.IsVisible = true;
            isValid = false;
        }
        else
        {
            lblErroCPF.IsVisible = false;
        }

        if (string.IsNullOrWhiteSpace(entryEmail.Text) || !IsValidEmail(entryEmail.Text))
        {
            lblErroEmail.IsVisible = true;
            isValid = false;
        }
        else
        {
            lblErroEmail.IsVisible = false;
        }

        if (!isValid) return;

        try
        {
            if (_isEdicao)
            {
                _cliente.Nome = entryNome.Text;
                _cliente.CPF = decimal.Parse(entryCPF.Text);
                _cliente.Email = entryEmail.Text;

                await App.Db.UpdateCliente(_cliente);
                await DisplayAlert("Sucesso", "Cliente atualizado com sucesso!", "OK");
            }
            else
            {
                var novoCliente = new Cliente
                {
                    Nome = entryNome.Text,
                    CPF = decimal.Parse(entryCPF.Text),
                    Email = entryEmail.Text,
                    DataCadastro = DateTime.Now
                };

                await App.Db.InsertCliente(novoCliente);
                await DisplayAlert("Sucesso", "Cliente cadastrado com sucesso!", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível salvar o cliente: {ex.Message}", "OK");
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnExcluirClicked(object sender, EventArgs e)
    {
        if (_cliente == null) return;

        try
        {
            bool podeExcluir = await App.Db.PodeExcluirCliente(_cliente.Id);

            if (!podeExcluir)
            {
                bool forcarExclusao = await DisplayAlert("Atenção!",
                    $"O cliente '{_cliente.Nome}' possui vínculos com animais. Deseja excluir mesmo assim? Isso também excluirá todos os vínculos.",
                    "Sim, excluir tudo", "Cancelar");

                if (!forcarExclusao) return;

                await App.Db.DeleteVinculosDoCliente(_cliente.Id);
            }
            else
            {
                bool confirmar = await DisplayAlert("Confirmar exclusão",
                    $"Deseja realmente excluir o cliente '{_cliente.Nome}'?",
                    "Sim", "Não");

                if (!confirmar) return;
            }

            await App.Db.DeleteCliente(_cliente.Id);
            await DisplayAlert("Sucesso", "Cliente excluído com sucesso!", "OK");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível excluir o cliente: {ex.Message}", "OK");
        }
    }
}
