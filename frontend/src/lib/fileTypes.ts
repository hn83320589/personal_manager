/** 上傳時可選的檔案類型，需與後端 FileInspector 允許的類型一致。 */
export const imageAccept = 'image/png,image/jpeg,image/webp,image/gif'
export const imageHint = 'JPG、PNG、WebP、GIF，可一次多張'

/** 附件只收輸出檔，不收 AI、PSD 等原始檔（ADR-012）。 */
export const documentAccept = '.pdf,.doc,.docx,.ppt,.pptx,.xls,.xlsx,.zip'
export const documentHint =
  'PDF、Word、PowerPoint、Excel、ZIP，單檔上限 50 MB（不接受 AI、PSD 原始檔）'
