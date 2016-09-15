using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using GygaxCore.DataStructures.DataStructures.Interfaces;
using GygaxCore.Interfaces;
using IImage = Emgu.CV.IImage;

namespace GygaxCore.DataStructures
{
    public abstract class Processor : Streamable, IProcessor
    {
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
                    Source.PropertyChanged += SourceUpdated;
                    SourceUpdated(value, null);
                }
            }
        }

        private bool _initialCall;
        
        public void SourceUpdated(object sender, EventArgs e)
        {
            if (!_initialCall)
            {
                Initial();
                _initialCall = true;
            }
            else
            {
                Update();
            }
        }

        public abstract void Initial();

        public abstract void Update();
    }
}
