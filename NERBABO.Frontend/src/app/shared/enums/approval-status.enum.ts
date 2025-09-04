export enum ApprovalStatus {
  NotSpecified = 'Não Especificado',
  Approved = 'Aprovado',
  Rejected = 'Reprovado'
}

export const ApprovalStatusOptions = [
  { label: ApprovalStatus.NotSpecified, value: ApprovalStatus.NotSpecified },
  { label: ApprovalStatus.Approved, value: ApprovalStatus.Approved },
  { label: ApprovalStatus.Rejected, value: ApprovalStatus.Rejected }
];