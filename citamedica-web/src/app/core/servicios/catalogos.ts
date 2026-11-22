import { inject, Service } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { API } from '../api.config';
import { MedicoOpcion } from '../modelos/catalogos';
import { CrearPaciente, Paciente } from '../modelos/paciente';

/**
 * Pacientes y médicos. Viven en el microservicio de Catálogos, en el puerto
 * 5110 — otro proceso distinto al de la agenda.
 */
@Service()
export class Catalogos {
  private readonly http = inject(HttpClient);

  private readonly pacientesUrl = `${API.catalogos}/api/Pacientes`;

  obtenerPacientes(): Promise<Paciente[]> {
    return firstValueFrom(this.http.get<Paciente[]>(this.pacientesUrl));
  }

  obtenerPaciente(pacienteId: number): Promise<Paciente> {
    return firstValueFrom(this.http.get<Paciente>(`${this.pacientesUrl}/${pacienteId}`));
  }

  crearPaciente(paciente: CrearPaciente): Promise<Paciente> {
    return firstValueFrom(this.http.post<Paciente>(this.pacientesUrl, paciente));
  }

  obtenerMedicos(): Promise<MedicoOpcion[]> {
    return firstValueFrom(this.http.get<MedicoOpcion[]>(`${API.catalogos}/api/Medicos`));
  }
}
