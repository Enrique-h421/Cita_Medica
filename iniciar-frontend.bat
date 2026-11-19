@echo off
title CitaMedica

REM Levanta el frontend de Angular y abre el navegador.
REM %~dp0 es la carpeta de este archivo, asi que funciona
REM aunque muevas el repositorio de sitio.
REM Para detenerlo: cierra esta ventana o presiona Ctrl+C.

cd /d "%~dp0citamedica-web"

call ng serve --open

pause
