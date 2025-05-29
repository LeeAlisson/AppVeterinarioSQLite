using AppVeterinarioSQLite.Models;
using System.Collections.ObjectModel;

namespace AppVeterinarioSQLite.Views;

public partial class ClientesPage : ContentPage
{
    private ObservableCollection<Cliente> _clientes;

    public ClientesPage()
    {
        InitializeComponent();
        _clientes = new ObservableCollection<Cliente>();
        collectionViewClientes.ItemsSource = _clientes;
        refreshView.Command = new Command(async () => await CarregarClientes());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarClientes();
    }

    private async Task CarregarClientes()
    {
        try
        {
            refreshView.IsRefreshing = true;

            var clientes = await App.Db.GetAllClientes();
            _clientes.Clear();

            foreach (var cliente in clientes)
            {
                _clientes.Add(cliente);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível carregar os clientes: {ex.Message}", "OK");
        }
        finally
        {
            refreshView.IsRefreshing = false;
        }
    }

    private async void OnPesquisaTextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            await CarregarClientes();
        }
    }

    private async void OnPesquisarClicked(object sender, EventArgs e)
    {
        try
        {
            string textoPesquisa = entryPesquisa.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(textoPesquisa))
            {
                await CarregarClientes();
                return;
            }

            var clientes = await App.Db.SearchClientes(textoPesquisa);
            _clientes.Clear();

            foreach (var cliente in clientes)
            {
                _clientes.Add(cliente);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro na pesquisa: {ex.Message}", "OK");
        }
    }

    private async void OnClienteSelecionado(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Cliente clienteSelecionado)
        {
            ((CollectionView)sender).SelectedItem = null;

            var parametros = new Dictionary<string, object>
            {
                { "cliente", clienteSelecionado }
            };

            await Shell.Current.GoToAsync("clientes/detalhes", parametros);
        }
    }

    private async void OnAdicionarClienteClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("clientes/detalhes");
    }

    private async void OnEditarCliente(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.BindingContext is Cliente cliente)
        {
            var parametros = new Dictionary<string, object>
            {
                { "cliente", cliente }
            };

            await Shell.Current.GoToAsync("clientes/detalhes", parametros);
        }
    }

    private async void OnExcluirCliente(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.BindingContext is Cliente cliente)
        {
            try
            {
                bool podeExcluir = await App.Db.PodeExcluirCliente(cliente.Id);

                if (!podeExcluir)
                {
                    bool forcarExclusao = await DisplayAlert("Atenção!",
                        $"O cliente '{cliente.Nome}' possui vínculos com animais. Deseja excluir mesmo assim? Isso também excluirá todos os vínculos.",
                        "Sim, excluir tudo", "Cancelar");

                    if (!forcarExclusao) return;

                    await App.Db.DeleteVinculosDoCliente(cliente.Id);
                }
                else
                {
                    bool confirmar = await DisplayAlert("Confirmar exclusão",
                        $"Deseja realmente excluir o cliente '{cliente.Nome}'?",
                        "Sim", "Não");

                    if (!confirmar) return;
                }

                await App.Db.DeleteCliente(cliente.Id);
                await DisplayAlert("Sucesso", "Cliente excluído com sucesso!", "OK");
                await CarregarClientes();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Não foi possível excluir o cliente: {ex.Message}", "OK");
            }
        }
    }
}
