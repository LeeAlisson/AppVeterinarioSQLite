namespace AppVeterinarioSQLite.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarEstatisticas();
    }

    private async Task CarregarEstatisticas()
    {
        try
        {
            var especies = await App.Db.GetAll();
            var animais = await App.Db.GetAllAnimais();
            var clientes = await App.Db.GetAllClientes();
            var vinculos = await App.Db.GetAllVinculos();

            lblTotalEspecies.Text = especies.Count.ToString();
            lblTotalAnimais.Text = animais.Count.ToString();
            lblTotalClientes.Text = clientes.Count.ToString();
            lblTotalVinculos.Text = vinculos.Count.ToString();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar estatísticas: {ex.Message}", "OK");
        }
    }

    private async void OnNovaEspecieClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("especies/detalhes");
    }

    private async void OnNovoAnimalClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("animais/detalhes");
    }

    private async void OnNovoClienteClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("clientes/detalhes");
    }

    private async void OnNovoVinculoClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("vinculos/detalhes");
    }
}
