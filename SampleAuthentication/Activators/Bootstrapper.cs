using Castle.Windsor;

namespace SampleAuthentication.Activators
{
	public class Bootstrapper
    {
        static Bootstrapper() => Container = new WindsorContainer();
        public static IWindsorContainer Container { get; }
        public static void Run() 
        {

        }
        public static void ShutDown() => Container?.Dispose();
    }
}
