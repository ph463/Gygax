using System.ComponentModel;
using System.Windows.Media;

namespace GygaxCore.Interfaces
{
    public interface IStreamable : INotifyPropertyChanged
    {
        /// <summary>
        /// ImageSource is only readable, as it is set via CvSource exclusively and then converted.
        /// </summary>
        ImageSource ImageSource { get; }

        Emgu.CV.IImage CvSource { get; set; }

        object Data { get; set; }

        void Close();

        void Save();

        string Location { get; }

        string Name { get; }
    }
}
