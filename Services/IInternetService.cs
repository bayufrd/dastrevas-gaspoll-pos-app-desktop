using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Services
{
    public interface IInternetService
    {
        Task<bool> IsInternetConnectedAsync();
        bool IsInternetConnected(); // Sinkron version
    }
}
