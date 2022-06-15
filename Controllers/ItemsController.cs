using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestWebAppl.Models;

namespace WebApi.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        public int PageSize = 15;
        private IRepository itemRepository;
        public ItemsController(IRepository _rep)
        {
            itemRepository= _rep;
        }
        //GET api/items
        [HttpGet]
        public IEnumerable<Item> Get() => itemRepository.Items;

        //GET api/items/F74349D5-52B4-4A4A-0382-08DA02C684C5
        [HttpGet("{id:guid}")]
        public ActionResult<Item> Get(Guid id)
        {
            var item = itemRepository.Items.First(g => g.Id == id);
            if (item != null)
                return item;

            return BadRequest();
        }
        //GET api/items/page/2
        [HttpGet("page/{productPage:int}")]
        public IEnumerable<Item> Get(int productPage)=>itemRepository.Items
                .Skip((productPage - 1) * PageSize)
                .Take(PageSize);
    

        //GET api/items/categories
        [HttpGet("categories")]
        public IQueryable<string> Categories()=> itemRepository.Items
            .Select(x => x.Category)
            .Distinct();

        //GET api/item/categories/Десерты
        [HttpGet("categories/{category}")]
        public IQueryable<Item> Get(string category) =>
            itemRepository.Items.Where(p => category == null || p.Category == category);


        //POST api/items/add/{item}
        [HttpPost("add")]
        public ActionResult<Item> Post(Item item)
        {
            try
            {
                if (item == null)
                {
                    return BadRequest();
                }
                var repitem =itemRepository.Items.FirstOrDefault(i => i.Id == item.Id);
                if (repitem != null)
                {
                    repitem.Description = item.Description;
                    repitem.Category = item.Category;
                    repitem.Price = item.Price;
                    repitem.Name = item.Name;
                    repitem.addedTime = item.addedTime;
                    itemRepository.SaveItem(repitem);
                }
                itemRepository.SaveItem(item);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }
        //DELETE api/items/F74349D5-52B4-4A4A-0382-08DA02C684C5
        [HttpDelete("{id:guid}")]
        public ActionResult<Item> Delete(Guid id)
        {
            try
            {
                var itemToDelete = itemRepository.Items.FirstOrDefault(i=>i.Id==id);
                if (itemToDelete == null)
                {
                    return NotFound($"Item with id={id} not found");
                }

                return itemRepository.DeleteItem(id);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting data");
            }
        }
    }
}