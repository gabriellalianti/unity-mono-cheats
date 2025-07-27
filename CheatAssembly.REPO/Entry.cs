using CheatAssembly.REPO.Core;
using UnityEngine;

namespace CheatAssembly.REPO
{
    public static class Entry
    {
        private static bool _booted;
        private static GameObject Load;

        public static void Loader()
        {
            if (_booted)
                return;
            _booted = true;
            
            Load = new GameObject();
            Load.AddComponent<Hax>();
            UnityEngine.Object.DontDestroyOnLoad(Load);
        }
    }
}
