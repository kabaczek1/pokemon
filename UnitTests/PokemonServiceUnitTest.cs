using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PokemonApi.Context;
using PokemonApi.Entities;
using PokemonApi.Models;
using PokemonApi.Services;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Type = PokemonApi.Entities.Type;

namespace UnitTests;

public class PokemonServiceUnitTest
{
    private readonly PokemonService _pokemonService;
    private readonly DatabaseContext _context;
    
    private readonly PokemonDto _charmanderDto = new()
    {
        Name = "Charmander",
        Type = Type.Fire,
        Attack = 15,
        Defense = 10,
        Health = 15,
        SpecialAttack = 15,
        SpecialDefense = 5,
        Speed = 15
    };
    
    private readonly PokemonDto _charmanderToUpdate = new()
    {
        Attack = 35,
        Defense = 5,
        Health = 15,
        SpecialAttack = 20,
        SpecialDefense = 10,
        Speed = 25
    };

    
    private readonly Pokemon _charmanderEntity = new()
    {
        Name = "Charmander",
        Type = Type.Fire,
        Attack = 15,
        Defense = 10,
        Health = 15,
        SpecialAttack = 15,
        SpecialDefense = 5,
        Speed = 15
    };

    private readonly PokemonDto _badCharmanderDto = new()
    {
        Name = new string('a', 260),
        Type = Type.Fire,
        Attack = 15,
        Defense = 10,
        Health = 15,
        SpecialAttack = 15,
        SpecialDefense = 5,
        Speed = 15
    };

    public PokemonServiceUnitTest()
    {
        var contextOptions = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase("PokemonServiceUnitTestsDb")
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        
        _context = new DatabaseContext(contextOptions);
        _pokemonService = new PokemonService(_context);

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }

    [Fact]
    public void Create_SinglePokemon_ReturnsTrue()
    {
        var isSuccess = _pokemonService.Create(_charmanderDto);

        Assert.True(isSuccess);
    }

    [Fact]
    public void Create_SinglePokemonWithBadName_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(_pokemonService.Create(_badCharmanderDto));
    }

    [Fact]
    public void Update_SinglePokemonWithBadName_ThrowsValidationException()
    {
        var createdPokemon = _context.Pokemons.Add(_charmanderEntity);
        _context.SaveChanges();

        Assert.Throws<ValidationException>(_pokemonService.Update(_badCharmanderDto));
    }

    [Fact]
    public void Delete_ExistingPokemon_ReturnsTrue()
    {
        var createdPokemon = _context.Pokemons.Add(_charmanderEntity);
        _context.SaveChanges();

        var id = createdPokemon.Entity.Id;
        
        Assert.True(_pokemonService.Delete(id));
        Assert.Null(_context.Pokemons.FirstOrDefault(p => p.Id == id));
    }

    [Fact]
    public void Update_ExistingPokemon_ReturnsUpdatedPokemon()
    {
        var createdPokemon = _context.Pokemons.Add(_charmanderEntity);
        _context.SaveChanges();
        
        var updatedPokemon = _pokemonService.Update(_charmanderToUpdate);
        
        Assert.NotNull(updatedPokemon);
    }
    
    [Fact]
    public void GetById_ExistingPokemon_ReturnsPokemonWithCorrectId()
    {
        var createdPokemon = _context.Pokemons.Add(_charmanderEntity);
        _context.SaveChanges();
        
        
        Assert.NotNull(_pokemonService.GetById(createdPokemon.Entity.Id));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-3)]
    [InlineData(100)]
    public void GetById_InvalidId_ThrowsArgumentException(int id)
    {
        Assert.Throws<ArgumentException>(_pokemonService.GetById(id));
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    public void GetAll_ExistingPokemon_ReturnsPokemonList(int count)
    {
        for(var i = 0; i < count; i++)
        {
            _context.Pokemons.Add(_charmanderEntity);
        }
        List<Pokemon> pokemons = _pokemonService.GetAll();
        Assert.Equal(pokemons.Count(), count);
    }
}