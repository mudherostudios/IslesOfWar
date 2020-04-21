using System.IO;

namespace MudHero
{
    public static class GameFile
    {
        public static void Save(string data, string location)
        {
            using (FileStream stream = new FileStream(location, FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(data);
                    writer.Close();
                }
            }
        }

        public static string Load(string location)
        {
            string data = "";

            if (File.Exists(location))
            {
                using (FileStream stream = new FileStream(location, FileMode.Open))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        data = reader.ReadString();
                        reader.Close();
                    }
                }
            }

            return data;
        }
    }
}
