using UnityEditor;

namespace ParrelSync
{
    [InitializeOnLoad]
    public class EditorQuit
    {
        static EditorQuit()
        {
            IsQuiting = false;
            EditorApplication.quitting += Quit;
        }

        /// <summary>
        ///     Is editor being closed
        /// </summary>
        public static bool IsQuiting { get; private set; }

        private static void Quit()
        {
            IsQuiting = true;
        }
    }
}