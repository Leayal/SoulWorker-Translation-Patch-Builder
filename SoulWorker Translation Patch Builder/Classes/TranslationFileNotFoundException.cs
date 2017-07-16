namespace SoulWorker_Translation_Patch_Builder.Classes
{
    class TranslationFileNotFoundException : System.IO.FileNotFoundException
    {
        public TranslationFileNotFoundException() : base() { }
        public TranslationFileNotFoundException(string message, string filename) : base(message, filename) { }
    }
}
