using Api.Dtos;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Api.Controllers
{
    [Route("api/v{version:apiVersion}/invoice")]
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize]
    public class InvoiceController : ControllerBase
    {
        private readonly InvoiceDbContext _dbContext;
        private readonly IConfiguration _configurationProvider;

        public InvoiceController(InvoiceDbContext dbContext, IConfiguration configurationProvider)
        {
            _dbContext = dbContext;
            _configurationProvider = configurationProvider;
        }

        [HttpGet("{invoiceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(int invoiceId)
        {
            var invoice = await _dbContext.Invoices.AsNoTracking().SingleOrDefaultAsync(u => u.Id == invoiceId);
            if (invoice == null) return NotFound();

            var invoiceDto = new InvoiceDto()
            {
                Hash = invoice.Hash,
                Id = invoice.Id,
                Status = Enum.GetName(typeof(InvoiceStatus), invoice.Status),
                Sum = invoice.Sum,
                PaymentLink = invoice.Status == (int)InvoiceStatus.Pending ? BuildPaymentUrl(invoice.Hash) : null
            };

            return Ok(invoiceDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody]CreateInvoiceDto invoiceDto)
        {
            var invoice = new Invoice()
            {
                Sum = invoiceDto.Sum,
                Status = (int)InvoiceStatus.Pending,
                Hash = GenerateHashCode(invoiceDto)
            };

            _dbContext.Invoices.Add(invoice);
            await _dbContext.SaveChangesAsync();

            var responseInvoice = new InvoiceDto()
            {
                Hash = invoice.Hash,
                Id = invoice.Id,
                Status = Enum.GetName(typeof(InvoiceStatus), invoice.Status),
                Sum = invoice.Sum,
                PaymentLink = BuildPaymentUrl(invoice.Hash)
            };

            return Ok(responseInvoice);
        }

        [HttpPut("cancelled/{invoiceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Cancelled(int invoiceId)
        {
            var invoice = await _dbContext.Invoices.SingleOrDefaultAsync(u => u.Id == invoiceId);
            if (invoice == null) return NotFound();

            invoice.Status = (int)InvoiceStatus.Cancelled;
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        private string GenerateHashCode(CreateInvoiceDto invoiceDto)
        {
            var obj = new
            {
                invoice = invoiceDto,
                date = DateTime.UtcNow.Ticks
            };

            using var sha256Hash = SHA256.Create();
            var hash = Convert.ToBase64String(
                    sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj))))
                   .TrimEnd('=').Replace('+', '-').Replace('/', '_');

            return hash;
        }

        private string BuildPaymentUrl(string hash)
        {
            var url = _configurationProvider.GetSection("PaymentUrl")?.Value;
            if (url != null)
            {
                return new Uri($"{url}/payment/{hash}").ToString();
            }

            return null;
        }
    }
}