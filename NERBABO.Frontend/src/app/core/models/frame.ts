export type Frame = {
  id: number;
  program: string;
  intervention: string;
  interventionType: string;
  operation: string;
  operationType: string;
  programLogoUrl?: string;
  financementLogoUrl?: string;
};

export type CreateFrameData = {
  program: string;
  intervention: string;
  interventionType: string;
  operation: string;
  operationType: string;
  programLogoFile?: File;
  financementLogoFile?: File;
};

export type UpdateFrameData = {
  id: number;
  program: string;
  intervention: string;
  interventionType: string;
  operation: string;
  operationType: string;
  programLogoFile?: File;
  financementLogoFile?: File;
  removeProgramLogo?: boolean;
  removeFinancementLogo?: boolean;
};
