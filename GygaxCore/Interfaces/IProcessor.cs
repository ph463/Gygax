using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GygaxCore.Interfaces;

namespace GygaxCore.DataStructures.DataStructures.Interfaces
{
    public interface IProcessor : IStreamable
    {
        IStreamable Source { get; set; }

        void SourceUpdated(object sender, EventArgs e);

        void Initial();

        void Update();
    }
}
