using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Core.People.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Enums;
using ZLinq;

namespace NERBABO.ApiService.Helper;

public class SeedDataHelp
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;

    // Environment variables for admin credentials
    private string email;
    private string userName;
    private string password;

    public SeedDataHelp(
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager,
    AppDbContext context,
    IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        email = configuration["Admin:email"] ?? "";
        userName = configuration["Admin:username"] ?? "";
        password = configuration["Admin:password"] ?? "";
    }

    public async Task InitializeAsync()
    {
        await InitializeRolesAsync();
        await InitializeAdminAsync();
        await InitializeTaxIvaAsync();
        await InitializeGeneralInfoAsync();
    }
    private async Task InitializeRolesAsync()
    {
        string[] roles =
            {
                Roles.Admin.ToString(),
                Roles.User.ToString(),
                Roles.FM.ToString(),
                Roles.CQ.ToString()
            };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private async Task InitializeAdminAsync()
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var existingPerson = await _context.People.FirstOrDefaultAsync(p => p.Id == 1);
            if (existingPerson != null)
            {
                await transaction.RollbackAsync();
                return;
            }

            var personAdmin = new Person
            {
                FirstName = "Admin",
                LastName = "Admin",
                NIF = "000000000"
            };

            var entity = _context.People.Add(personAdmin);
            await _context.SaveChangesAsync();

            var admin = new User
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                PersonId = entity.Entity.Id,
                Person = personAdmin,
            };

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var result = await _userManager.CreateAsync(admin, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRolesAsync(admin, [Roles.Admin.ToString(), Roles.User.ToString()]);
                    await transaction.CommitAsync();
                }
                else
                {
                    Console.WriteLine($"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    await transaction.RollbackAsync();
                }
            }
            else
            {
                await transaction.RollbackAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in InitializeAdminAsync: {ex.Message}");
            await transaction.RollbackAsync();
            throw;
        }
    }
    private async Task InitializeGeneralInfoAsync()
    {
        if (await _context.GeneralInfo.AnyAsync())
            return;

        var generalInfo = new GeneralInfo
        {
            Id = 1,
            Designation = "NERBA-Associação Empresarial do Distrito de Bragança",
            IvaId = 1,
            Site = "Avenida das Cantarias, n.º 140, 5300-107 Bragança",
            HourValueTeacher = 10.5f,
            HourValueAlimentation = 6.0f,
            BankEntity = "Crédito Agrícola",
            Iban = "PT50004521914029554091581",
            Nipc = "502280344",
            Logo = "",
            Email = "nerba@nerba.pt",
            Slug = "NERBA",
            PhoneNumber = "273304630",
            Website = "www.nerba.pt",
            InsurancePolicy = "11706300-VICTORIA Acidentes Pessoais",
            FacilitiesCharacterization = "Sala equipada com mesas, cadeiras, para formandos/as e formador/a, videoprojetor, quadro didax e marcadores. Material para a realização de exercicios práticos, ajustado à área de formação em causa"
        };

        _context.GeneralInfo.Add(generalInfo);
        await _context.SaveChangesAsync();
    }

    private async Task InitializeTaxIvaAsync()
    {
        if (await _context.Taxes.AnyAsync())
            return;

        var taxes = new List<Tax>
            {
                new ("Regime de Isenção", 0, true, TaxEnum.IVA),
                new ("Sujeto à taxa de 23%", 23, true, TaxEnum.IVA),
                new ("Rentenção na Fonte de IRS à taxa de 25%", 25, true, TaxEnum.IRS),
                new ("Rentenção na Fonte de IRS à taxa de 23%", 23, true, TaxEnum.IRS),
                new ("Rentenção na Fonte de IRS à taxa de 11,5%", 12, true, TaxEnum.IRS),
                new ("Rentenção na Fonte de IRS à taxa de 16,5%", 17, true, TaxEnum.IRS),
                new ("Rentenção na Fonte de IRS à taxa de 20%", 20, true, TaxEnum.IRS)
            };

        _context.Taxes.AddRange(taxes);
        await _context.SaveChangesAsync();
    }
}
