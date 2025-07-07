import { Module } from './module';

export interface Course {
  id: number;
  frameId: number;
  frameProgram: string;
  title: string;
  objectives: string;
  destinators: string[];
  totalDuration: number;
  status: string;
  area: string;
  minHabilitationLevel: string;
  createdAt: string;
  modules: Module[];
  remainingDuration: number;
  actionsQnt: number;
  modulesQnt: number;
}
