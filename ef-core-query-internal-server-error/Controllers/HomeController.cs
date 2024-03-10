using System.Transactions;
using ef_core_query_internal_server_error.Data;
using ef_core_query_internal_server_error.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ef_core_query_internal_server_error.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }


    /// <summary>
    /// This method will throw an internal server error. because we are using transaction scope for the whole method
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> ReproduceInternalServerError(string name)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        try
        {
            var yolo = await ReproduceGetOrCreate(name);
            return Ok(yolo);
        }
        catch (Exception e)
        {
            return Ok("__ MY ERROR __" + e.Message);
        }
        finally
        {
            scope.Complete();
        }
    }


    /// <summary>
    /// This return the correct result, because we are using transaction scope for the only part that we need to be in a transaction, till this does not meet the requirements of the business
    /// This method should check the name and if it does not exist, it should create a new one, but if it exists, it should return the existing one
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> JustCreateWithRegularException(string name)
    {
        try
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var yolo = await ReproduceGetOrCreate(name);
            scope.Complete();
            return Ok(yolo);
        }
        catch (Exception e)
        {
            return Ok("__ MY ERROR __" + e.Message);
        }
    }

    /// <summary>
    ///  This return the correct result, because we are using transaction scope for the only part that we need to be in a transaction, firstly we are not using global transaction scope,
    /// and secondly we are using the transaction scope for the only part that we need to be in a transaction
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> ResolveRegularAction(string name)
    {
        try
        {
            var yolo = await ResolveGetOrCreateAsync(name);
            return Ok(yolo);
        }
        catch (Exception e)
        {
            return BadRequest("__ MY ERROR __" + e.Message);
        }
    }


    private async Task<Yolo> ResolveGetOrCreateAsync(string name)
    {
        try
        {
            await CreateConcurrency(name);
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var model = await CreateYoloAsync(name);
            scope.Complete();
            return model;
        }
        catch (DbUpdateException e)
        {
            return _context.Yolos.First(x => x.Name == name);
        }
    }


    private async Task<Yolo> ReproduceGetOrCreate(string name)
    {
        try
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            await CreateConcurrency(name);
            var model = await CreateYoloAsync(name);
            scope.Complete();
            return model;
        }
        catch (DbUpdateException e)
        {
            return _context.Yolos.First(x => x.Name == name);
        }
    }

    private async Task CreateConcurrency(string name)
    {
        var model = new Concurrency
        {
            Name = name,
        };
        _context.Concurrencies.Add(model);
        await _context.SaveChangesAsync();
    }

    private async Task<Yolo> CreateYoloAsync(string name)
    {
        var yolo = new Yolo
        {
            Name = name,
        };

        _context.Yolos.Add(yolo);
        await _context.SaveChangesAsync();
        return yolo;
    }
}