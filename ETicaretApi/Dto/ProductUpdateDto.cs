using System.ComponentModel.DataAnnotations;

namespace ETicaretApi.Dto
{
    public class ProductUpdateDto
    {
        [Range(0,100)]
        public decimal PriceIncreasePercentage { get; set; }

        [Range(0, int.MaxValue)]
        public int StockIncrease { get; set; }
    }
}
