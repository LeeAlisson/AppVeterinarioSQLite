namespace AppVeterinarioSQLite;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("especies/detalhes", typeof(Views.EspecieDetalhesPage));
        Routing.RegisterRoute("animais/detalhes", typeof(Views.AnimalDetalhesPage));
        Routing.RegisterRoute("clientes/detalhes", typeof(Views.ClienteDetalhesPage));
        Routing.RegisterRoute("vinculos/detalhes", typeof(Views.VinculoDetalhesPage));
    }
}
