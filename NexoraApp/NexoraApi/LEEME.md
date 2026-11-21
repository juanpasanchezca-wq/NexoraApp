# NexoraApi (backend de Nexora)

API mínima en .NET con SQLite, empaquetada en Docker.

## Endpoints
- `GET  /api/Tarea`  → lista las tareas del servidor
- `POST /api/Tarea`  → recibe una lista de tareas (JSON) y las guarda o actualiza

## Ejecutar con Docker (en esta carpeta, con Docker Desktop abierto)
```
docker build -t nexora-api .
docker run -d --name nexora-api -p 8080:8080 -v nexora-datos:/data nexora-api
```
Probar en el navegador: http://localhost:8080/api/Tarea  (al inicio devuelve `[]`)

## Ejecutar sin Docker (solo para probar)
```
dotnet run --urls http://localhost:8080
```

## Comandos útiles
```
docker ps                      # ver contenedores en ejecución
docker logs nexora-api         # ver el registro del servidor
docker stop nexora-api         # detener
docker rm nexora-api           # eliminar el contenedor
```
