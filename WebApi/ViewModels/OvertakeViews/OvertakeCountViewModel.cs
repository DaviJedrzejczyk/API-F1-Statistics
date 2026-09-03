using System.Text.Json.Serialization;

namespace WebApi.ViewModels.OvertakeViews
{
    public class OvertakeCountViewModel
    {
        [JsonPropertyName("numbers_of_overtakes")]
        public int NumbersOfOvertakes { get; set; }
        public OvertakeCountViewModel(int numberOfOvertakes)
        {
            NumbersOfOvertakes = numberOfOvertakes;
        }
    }
}
