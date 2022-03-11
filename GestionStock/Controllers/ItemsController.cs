#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionStock.Data;
using GestionStock.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Newtonsoft.Json;

namespace GestionStock.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ACTEAM_StockContext _context;
        private LogWriter _logWriter;
        private static string LogString = String.Empty;
        private HttpClient _HttpClient;
        private IConfiguration _configuration;
        public ItemsController(ACTEAM_StockContext context,IConfiguration configuration)
        {
            _logWriter = new LogWriter();
            _HttpClient = new HttpClient();
            _context = context;
            _configuration = configuration;
        }

        // GET: Items
        public async Task<IActionResult> Index()
        {
            var items = await _context.Items.ToListAsync();
            return View(items);
        }

        //Search
        public async Task<IActionResult> Search(string searchString)
        {
            var items = from m in _context.Items
                        select m;

            if (!String.IsNullOrEmpty(searchString))
            {
                items = items.Where(s => s.Designation.ToLower().Contains(searchString.ToLower()) || s.Reference.ToLower().Contains(searchString.ToLower()) || s.Marque.ToLower().Contains(searchString.ToLower()));
            }

            return View("iNDEX", await items.ToListAsync());
        }

        // GET: Items/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        // GET: Items/Create
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Items/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([Bind("Id,Marque,Designation,Reference,Quantity")] Item item)
        {
            if (ModelState.IsValid)
            {
                _context.Add(item);
                await _context.SaveChangesAsync();
                LogString = $"Create : Items = ID : {item.Id} | Marque : {item.Marque} | Designation : {item.Designation} | Reference : {item.Reference} | Quantity : {item.Quantity}";
                _logWriter.LogWrite(LogString);
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }
        // GET: Items/Edit/5
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items.FindAsync(id);
            LogString = $"Original Item : Items = ID : {item.Id} | Marque : {item.Marque} | Designation : {item.Designation} | Reference : {item.Reference} | Quantity : {item.Quantity} \n";
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        // POST: Items/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]

        public async Task<IActionResult> Edit(int id, [Bind("Id,Marque,Designation,Reference,Quantity")] Item item)
        {
            if (id != item.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if (item.Quantity <= 3)
                    {
                        string ItemContent = $"ID :{item.Id}|Marque : {item.Marque}|Désignation : {item.Designation}|Ref : {item.Reference}| Quantite : {item.Quantity}";
                        await SendStockMessage(ItemContent);
                    }
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                    LogString += $"  -Updated Item : Items = ID : {item.Id} | Marque : {item.Marque} | Designation : {item.Designation} | Reference : {item.Reference} | Quantity : {item.Quantity} \n";
                    _logWriter.LogWrite(LogString);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(item.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        // GET: Items/Delete/5
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Items.FindAsync(id);
            LogString = $"Delete :  Item : Items = ID : {item.Id} | Marque : {item.Marque} | Designation : {item.Designation} | Reference : {item.Reference} | Quantity : {item.Quantity}";
            _logWriter.LogWrite(LogString);
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //Send message to webhook using post
        public async Task<IActionResult> SendStockMessage(string message)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(_configuration["URI:webhook"]);
            Message body = new Message();
            //Body.text = messageCard JSON teams webhook
            // body.text = "\"@type\": \"MessageCard\",\"@context\": \"http://schema.org/extensions\",\"themeColor\": \"0072C6\",\"title\": \"Stock Alert\",\"sections\":[{\"activityTitle\": \"Stock Alert\",\"activitySubtitle\": \"Stock Alert\",\"activityImage\": \"https://i.ibb.co/0jqQXxL/alert.png\",\"facts\":[{\"name\": \"Marque\",\"value\": \"" + message + "\"}]}]}";
            body.@type = "MessageCard";
            body.@context = "http://schema.org/extensions";
            body.themeColor = "0072C6";
            body.title = "Stock Alert";
            body.activityTitle = "Stock Alert";
            body.activitySubtitle = "Stock Alert";
            body.activityImage = "https://i.ibb.co/0jqQXxL/alert.png";
            body.name = "Objet";
            body.text = message;
            string serializeJson = JsonConvert.SerializeObject(body);
            StringContent content = new StringContent(serializeJson, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(client.BaseAddress, content);
            var contentresp = response.Content;
            using (var reader = new StreamReader(await contentresp.ReadAsStreamAsync()))
            {
                var result = await reader.ReadToEndAsync();
            }
            return Ok();
        }
        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.Id == id);
        }
    }
}

public class Message
{
    public string @type { get; set; }
    public string @context { get; set; }

    public string themeColor { get; set; }

    public string title { get; set; }

    public string text { get; set; }

    public string activityTitle { get; set; }

    public string activitySubtitle { get; set; }

    public string activityImage { get; set; }

    public string name { get; set; }

    public string value { get; set; }


}
