/**
 * Brand Configuration for Baltistan Dry Fruits E-Commerce
 * All values are configurable via environment variables or sensible defaults.
 */

export interface BrandConfig {
  name: string
  tagline: string
  origin: string
  announcement: string
  contactEmail: string
  contactPhone: string
  currency: string
  shippingThreshold: number
  description: string
}

export const brandConfig: BrandConfig = {
  name: import.meta.env.VITE_BRAND_NAME || 'Karakoram Harvest',
  tagline: import.meta.env.VITE_BRAND_TAGLINE || 'Organic Dry Fruit and Nuts',
  origin: 'Skardu & Hunza Valley, Gilgit-Baltistan, Pakistan',
  announcement: import.meta.env.VITE_ANNOUNCEMENT || '🏔️ Karakoram Harvest: Direct from Skardu & Hunza Valley • Free nationwide shipping over PKR 4,000',
  contactEmail: import.meta.env.VITE_CONTACT_EMAIL || 'salam@karakoramharvest.pk',
  contactPhone: import.meta.env.VITE_CONTACT_PHONE || '+92 5815 450123',
  currency: 'PKR',
  shippingThreshold: 4000,
  description: 'Hand-picked organic dry fruit and nuts harvested from centuries-old ancestral orchards across Skardu, Shigar, and Hunza. Naturally sun-dried, pesticide-free, and glacial-melt nourished.',
}
