using ElectroGestion.Context;
using ElectroGestion.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ElectroGestion.Services;

public class AutenticacionService(
    IDbContextFactory<Contexto> contextFactory,
    IPasswordHasher<Usuario> passwordHasher)
{
    public async Task<Usuario?> AutenticarAsync(string usuarioLogin, string password)
    {
        await using var contexto = await contextFactory.CreateDbContextAsync();
        var usuario = await contexto.Usuarios
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.UsuarioLogin == usuarioLogin);

        if (usuario is null)
        {
            return null;
        }

        var resultado = passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, password);
        return resultado == PasswordVerificationResult.Failed ? null : usuario;
    }
}
