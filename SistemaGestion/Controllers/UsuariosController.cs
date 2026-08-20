using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaGestion.Models;
using SistemaGestion.Services.Interfaces;
using SistemaGestion.ViewModels;

namespace SistemaGestion.Controllers;

[Authorize(Roles = "Administrador")]
public class UsuariosController : Controller
{
    private readonly IUsuarioService _usuarioService;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public UsuariosController(IUsuarioService usuarioService, IPasswordHasher<Usuario> passwordHasher)
    {
        _usuarioService = usuarioService;
        _passwordHasher = passwordHasher;
    }

    public async Task<IActionResult> Index()
    {
        var usuarios = await _usuarioService.ListarAsync();
        return View(usuarios);
    }

    public IActionResult Crear() => View(new UsuarioCrearViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(UsuarioCrearViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        var usuario = new Usuario
        {
            NombreUsuario = modelo.NombreUsuario,
            NombreCompleto = modelo.NombreCompleto,
            Email = modelo.Email,
        };
        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, modelo.Password);

        try
        {
            await _usuarioService.CrearAsync(usuario, modelo.RolId);
            TempData["Mensaje"] = "Usuario creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }
}
