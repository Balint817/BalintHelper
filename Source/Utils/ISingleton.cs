namespace Celeste.Mod.BalintHelper.Utils
{
    public interface ISingleton<T> where T : class, new()
    {
        private static T? _instance;
        public static T Instance
        {
            get
            {
                return _instance ??= new T();
            }
        }
    }
}