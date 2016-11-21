using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        string Location { get; }

        string Name { get; }
    }
}
