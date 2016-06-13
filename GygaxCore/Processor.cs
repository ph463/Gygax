using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using GygaxCore.Interfaces;
using IImage = Emgu.CV.IImage;

namespace GygaxCore
{
    public abstract class Processor : Streamable, IProcessor
    {
        //public event PropertyChangedEventHandler PropertyChanged;

        //public ImageSource ImageSource { get; }

        //public IImage CvSource { get; set; }

        //public object Data { get; set; }

        public override void Close() { }

        private IStreamable _source;
        public IStreamable Source
        {
            get { return _source; }
            set
            {
                _source = value;
                if (_source != null)
                {
                    SourceUpdated(_source, EventArgs.Empty);
                }
            }
        }

        private readonly Thread _initialThread;
        private readonly Thread _updateThread;

        private bool _initialCall;

        protected Processor()
        {
            _initialThread = new Thread(Initial);
            _updateThread = new Thread(Update);
        }

        public void SourceUpdated(object sender, EventArgs e)
        {
            if (!_initialCall)
            {
                _initialCall = true;
                _initialThread.Start();
            }
            else
            {
                _updateThread.Start();
            }
        }

        public abstract void Initial();

        public abstract void Update();
    }
}
