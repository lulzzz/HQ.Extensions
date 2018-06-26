using System.Threading.Tasks;
using reactive.pipes;
using reactive.pipes.tests.Fakes;

namespace reactive.tests.Fakes
{
    public class SuccessHandler : IConsume<IEvent>
    {
        public int Handled { get; private set; }

        public Task<bool> HandleAsync(IEvent message)
        {
            Handled++;
            return Task.FromResult(true);
        }
    }
}