using AutoLavadoApp.Views.Empleado;

namespace AutoLavadoApp.Views.Empleado;

public class EmpleadoTabbedPage : TabbedPage
{
    public EmpleadoTabbedPage()
    {
        Title = "BRIZZO";

        BarBackgroundColor = Color.FromArgb("#FFFFFF");
        BarTextColor = Color.FromArgb("#94A3B8");
        SelectedTabColor = Color.FromArgb("#2563EB");
        UnselectedTabColor = Color.FromArgb("#94A3B8");

        Children.Add(CrearTab(new EmpleadoHomePage(), "\uf015"));
        Children.Add(CrearTab(new EmpleadoCitasPage(), "\uf328"));
        Children.Add(CrearTab(new EmpleadoCuentaPage(), "\uf007"));
    }

    private static NavigationPage CrearTab(Page page, string glyph)
    {
        var icon = new FontImageSource
        {
            Glyph = glyph,
            FontFamily = "FontAwesomeSolid",
            Size = 18,
            Color = Color.FromArgb("#2563EB")
        };

        return new NavigationPage(page)
        {
            Title = " ",
            IconImageSource = icon,
            BarBackgroundColor = Color.FromArgb("#FFFFFF"),
            BarTextColor = Color.FromArgb("#0B1F4D")
        };
    }
}
