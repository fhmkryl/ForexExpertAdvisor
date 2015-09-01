using ExpertAdvisor.Advisor;
using NQuotes;

namespace ExpertAdvisor
{
    public class ExpertAdvisor : MqlApi
    {
        public override int start()
        {
            StartInternal();

            return 1;
        }

        private void StartInternal()
        {
            AdvisorFactory advisorFactory = new AdvisorFactory();
            Advisor.ExpertAdvisor advisor = advisorFactory.CreateExpertAdvisor(AdvisorType.Scalper, this);

            advisor.Execute();
        }
    }
}