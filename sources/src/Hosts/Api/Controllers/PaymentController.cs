using Api.Dtos;
using Domain;
using Infrastructure;
using Infrastructure.KafkaProducer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Api.Controllers
{
    [Route("payment")]
    public class PaymentController : Controller
    {
        private readonly InvoiceDbContext _dbContext;
        private readonly IKafkaProducer _kafkaProducer;

        public PaymentController(
            InvoiceDbContext dbContext,
            IKafkaProducer kafkaProducer)
        {
            _dbContext = dbContext;
            _kafkaProducer = kafkaProducer;
        }

        [HttpGet("{hash}")]
        public async Task<ActionResult> Index(string hash)
        {
            var invoice = await _dbContext.Invoices.AsNoTracking().SingleOrDefaultAsync(u => u.Hash == hash);
            if (invoice == null) return NotFound();

            var invoiceDto = new InvoiceDto()
            {
                Id = invoice.Id,
                Sum = invoice.Sum,
                Status = Enum.GetName(typeof(InvoiceStatus), invoice.Status),
            };

            return View(invoiceDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Pay(IFormCollection collection)
        {
            var invoice = await _dbContext.Invoices.SingleAsync(u => u.Id == int.Parse(collection["Id"].ToString()));

            try
            {
                invoice.Status = (int)InvoiceStatus.Paid;
                
                var invoiceDto = new InvoiceDto()
                {
                    Id = invoice.Id,
                    Sum = invoice.Sum,
                    Status = Enum.GetName(typeof(InvoiceStatus), invoice.Status),
                };

                await _kafkaProducer.PublishMessageAsync(JsonConvert.SerializeObject(invoiceDto));

                await _dbContext.SaveChangesAsync();

                return RedirectToAction("Index", new { hash = invoice.Hash });
            }
            catch
            {
                return RedirectToAction("Index", new { hash = invoice.Hash });
            }
        }
    }
}
