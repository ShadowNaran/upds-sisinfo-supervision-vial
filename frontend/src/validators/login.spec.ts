import { describe, expect, it } from 'vitest'
import { tieneErrores, validarLogin } from './login'

describe('validarLogin hu-01 tarea 3', () => {
  it('acepta credenciales completas', () => {
    const errores = validarLogin({ username: 'supervisor', password: 'Contrasena.123' })
    expect(tieneErrores(errores)).toBe(false)
  })

  it('rechaza usuario vacio', () => {
    const errores = validarLogin({ username: '', password: 'Contrasena.123' })
    expect(errores.username).toBe('El usuario es obligatorio.')
  })

  it('rechaza usuario con menos de 3 caracteres', () => {
    const errores = validarLogin({ username: 'ab', password: 'Contrasena.123' })
    expect(errores.username).toContain('al menos 3')
  })

  it('rechaza contrasena vacia', () => {
    const errores = validarLogin({ username: 'supervisor', password: '' })
    expect(errores.password).toBe('La contrasena es obligatoria.')
  })

  it('rechaza contrasena con menos de 6 caracteres', () => {
    const errores = validarLogin({ username: 'supervisor', password: '12345' })
    expect(errores.password).toContain('al menos 6')
  })

  it('trata espacios como usuario invalido', () => {
    const errores = validarLogin({ username: '   ', password: 'Contrasena.123' })
    expect(errores.username).toBe('El usuario es obligatorio.')
  })
})