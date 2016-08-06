
namespace WebCalculatorService
{
    using Owin;
    public interface IOwinAppBuilder
    {
        void Configuration(IAppBuilder appBuilder);
    }
}
