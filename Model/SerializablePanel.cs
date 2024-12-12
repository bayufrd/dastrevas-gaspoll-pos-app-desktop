using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Model
{
    [Serializable]
    public class SerializablePanel : Panel, ISerializable
    {
        public SerializablePanel() { }

        public SerializablePanel(SerializationInfo info, StreamingContext context)
        {
            Width = info.GetInt32("Width");
            Height = info.GetInt32("Height");
        }

        public SerializablePanel(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Width", Width);
            info.AddValue("Height", Height);
        }
    }
}
