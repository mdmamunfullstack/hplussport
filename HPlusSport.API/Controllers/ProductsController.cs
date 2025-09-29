﻿using Asp.Versioning;
using HPlusSport.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HPlusSport.API.Models.QueryParameters;

namespace HPlusSport.API.Controllers
{
    [ApiVersion(1.0)]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ShopContext _context;
        public ProductsController(ShopContext shopContext)
        {
            _context = shopContext;

            _context.Database.EnsureCreated();
        }

        /*
        [HttpGet]
        public IEnumerable<Product> GetAllProducts()
        {
            return _context.Products.ToArray();
        }
        */

        [HttpGet]
        [MapToApiVersion("1.0")]

        public async Task<ActionResult<IEnumerable<Product>>> GetAllProductsV1([FromQuery]ProductQueryParameter queryParameters)
        {
            IQueryable<Product> products  = _context.Products;
            if (queryParameters.MinPrice != null )
            {
                products = products.Where(p => p.Price >= queryParameters.MinPrice.Value);

            }
            if (queryParameters.MaxPrice != null)
            {
                products =products.Where(p => p.Price <= queryParameters.MaxPrice.Value);

            }
            if (!string.IsNullOrEmpty(queryParameters.SearchTerm))
            {
                products = products.Where(p => p.Sku.ToLower().Contains(queryParameters.SearchTerm.ToLower()) ||
                                               p.Name.ToLower().Contains(queryParameters.SearchTerm.ToLower()));
            }

            if (!string.IsNullOrEmpty(queryParameters.Sku))
            {
                products = products.Where(p => p.Sku == queryParameters.Sku);

            }
            if (!string.IsNullOrEmpty(queryParameters.Name))
            {
                products = products.Where(p => p.Name.ToLower().Contains(queryParameters.Name.ToLower()));
            }
            if (!string.IsNullOrEmpty(queryParameters.SortBy))
            {

                if (typeof(Product).GetProperty(queryParameters.SortBy) != null)
                {
                    products = products.OrderByCustom(queryParameters.SortBy, queryParameters.SortOrder);

                }

            }




            products = products.Skip(queryParameters.Size * (queryParameters.Page - 1))
                               .Take(queryParameters.Size);

            return Ok(await products.ToArrayAsync());
        }


        [HttpGet]
        [MapToApiVersion("2.0")]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProductsV2([FromQuery] ProductQueryParameter queryParameters)
        {
            IQueryable<Product> products = _context.Products.Where(p => p.IsAvailable == true);
            if (queryParameters.MinPrice != null)
            {
                products = products.Where(p => p.Price >= queryParameters.MinPrice.Value);

            }
            if (queryParameters.MaxPrice != null)
            {
                products = products.Where(p => p.Price <= queryParameters.MaxPrice.Value);

            }
            if (!string.IsNullOrEmpty(queryParameters.SearchTerm))
            {
                products = products.Where(p => p.Sku.ToLower().Contains(queryParameters.SearchTerm.ToLower()) ||
                                               p.Name.ToLower().Contains(queryParameters.SearchTerm.ToLower()));
            }

            if (!string.IsNullOrEmpty(queryParameters.Sku))
            {
                products = products.Where(p => p.Sku == queryParameters.Sku);

            }
            if (!string.IsNullOrEmpty(queryParameters.Name))
            {
                products = products.Where(p => p.Name.ToLower().Contains(queryParameters.Name.ToLower()));
            }
            if (!string.IsNullOrEmpty(queryParameters.SortBy))
            {

                if (typeof(Product).GetProperty(queryParameters.SortBy) != null)
                {
                    products = products.OrderByCustom(queryParameters.SortBy, queryParameters.SortOrder);

                }

            }




            products = products.Skip(queryParameters.Size * (queryParameters.Page - 1))
                               .Take(queryParameters.Size);

            return Ok(await products.ToArrayAsync());
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpGet("available")]
        public async Task<ActionResult> GetAvailableProducts()
        {
            var availableProducts = await _context.Products.Where(x => x.IsAvailable == true).ToArrayAsync();
            if (availableProducts == null)
            {
                return NotFound();
            }
            return Ok(availableProducts);
        }

        [HttpPost]
        public async Task<ActionResult> PostProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                "GetProduct",
                new { id = product.Id },
                product
                );
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(x => x.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if(product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(product);
        }

        [HttpPost("Delete")]
        public async Task<ActionResult> DeleteMultiple([FromQuery]int[] ids)
        {
            var products = new List<Product>();
            foreach(var id in ids)
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null) return NotFound();
                products.Add(product);
            }

            _context.Products.RemoveRange(products);
            await _context.SaveChangesAsync();

            return Ok(products);
        }
    }
}
