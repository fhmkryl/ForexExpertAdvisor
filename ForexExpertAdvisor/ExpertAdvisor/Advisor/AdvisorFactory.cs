using System.ComponentModel;
using NQuotes;

namespace ExpertAdvisor.Advisor
{
    public class AdvisorFactory
    {
        public ExpertAdvisor CreateExpertAdvisor(AdvisorType advisorType, MqlApi api) {
            switch(advisorType) {
                case AdvisorType.Scalper:
                    return new ScalperAdvisor(api);
                default:
                    throw new InvalidEnumArgumentException(string.Format("Invalid advisor type:{0}", advisorType));
            }
        }
    }

    public enum AdvisorType
    {
        Scalper
    }
}
