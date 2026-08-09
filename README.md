# Simulación Monte-Carlo SIRD

Simulación Monte-Carlo de un modelo epidemiológico SIRD sobre grilla 2D (1000×1000), con versión secuencial y paralela usando TPL (`Parallel.For`).

## Estructura

- `src/Epidemia.Core/` — Biblioteca compartida: modelo SIRD, RNG determinista, grilla, estadísticas.
- `src/Epidemia.Secuencial/` — Versión secuencial (1 hilo).
- `src/Epidemia.Paralelo/` — Versión paralela con TPL.
- `src/Epidemia.Benchmark/` — Experimentos de strong scaling + visualización.
- `resultados/` — CSVs, gráfica de speed-up, animación GIF (generados al ejecutar).

## Requisitos

- .NET 9 SDK
- El proyecto de benchmark descarga automáticamente `SixLabors.ImageSharp` vía NuGet.

## Compilar

```bash
dotnet build EpidemiaMonteCarlo.slnx -c Release
```

## Ejecutar

Todos los comandos se ejecutan desde la raíz del proyecto (`monte-carlo/`).

### Versión secuencial

```bash
dotnet run --project src/Epidemia.Secuencial -c Release
```

### Versión paralela

```bash
dotnet run --project src/Epidemia.Paralelo -c Release -- --threads 8
```

### Benchmark completo (scaling + animación)

```bash
dotnet run --project src/Epidemia.Benchmark -c Release
```

Genera en `resultados/`:
- `benchmark_times.csv` — Tiempos por número de hilos.
- `speedup.svg` — Gráfica de speed-up vs. hilos (ideal vs. real).
- `epidemia_animacion.gif` — Animación side-by-side secuencial vs. paralelo.

## Parámetros configurables

| Parámetro | Default | Descripción |
|---|---|---|
| `--width` | 1000 | Ancho de la grilla |
| `--height` | 1000 | Alto de la grilla |
| `--days` | 365 | Días de simulación |
| `--replicas` | 30 | Número de réplicas Monte-Carlo |
| `--seed` | 42 | Semilla base del RNG |
| `--beta` | 0.25 | Prob. de contagio por contacto |
| `--gamma` | 0.1429 | Prob. de recuperación (1/7) |
| `--mu` | 0.005 | Prob. de muerte |
| `--threads` | auto | Hilos (solo versión paralela) |
| `--initial-infected` | 10 | Celdas infectadas iniciales |

## Modelo SIRD

SIRD (Susceptible → Infectado → Recuperado/Difunto) en grilla 2D con vecindad de Moore (8 vecinos).

- **Contagio**: P(S→I) = 1 − (1 − β)^k, donde k = vecinos infectados
- **Muerte**: P(I→D) = μ
- **Recuperación**: P(I→R) = γ
- **R y D**: estados absorbentes

RNG determinista por coordenada: mismo resultado con cualquier número de hilos.

## Colores de la animación

| Color | Estado |
|---|---|
| 🟩 Verde | Susceptible |
| 🟥 Rojo | Infectado |
| 🟦 Azul | Recuperado |
| ⬛ Gris | Difunto |

## Validación

Ejecutar con la misma semilla y 1 hilo en ambas versiones produce resultados idénticos:

```bash
dotnet run --project src/Epidemia.Secuencial -c Release -- --replicas 1 --seed 42
dotnet run --project src/Epidemia.Paralelo -c Release -- --replicas 1 --seed 42 --threads 1
```

Comparar los CSVs generados: deben ser idénticos.
