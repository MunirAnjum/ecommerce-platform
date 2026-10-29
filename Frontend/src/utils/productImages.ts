/**
 * Product & Category image resolver for Karakoram Harvest.
 * Uses persistent Cloudinary URLs with verified local image fallbacks.
 */

export const DEFAULT_PRODUCT_FALLBACK = '/images/dry_fruits.jpg'

/**
 * Resolves the display image URL for a product.
 * Returns the Cloudinary secure URL if present, otherwise returns the verified local fallback.
 */
export function getProductImageUrl(
  targetOrUrl?: string | { imageUrl?: string | null; name?: string } | null,
  _categoryName?: string,
  _explicitUrl?: string,
  _productId?: string
): string {
  // If a product object was provided
  if (targetOrUrl && typeof targetOrUrl === 'object') {
    if (targetOrUrl.imageUrl && targetOrUrl.imageUrl.trim().length > 0) {
      return targetOrUrl.imageUrl.trim()
    }
    return DEFAULT_PRODUCT_FALLBACK
  }

  // If a string was provided
  if (typeof targetOrUrl === 'string' && targetOrUrl.trim().length > 0) {
    const trimmed = targetOrUrl.trim()
    // Check if it's already a full URL or absolute path
    if (trimmed.startsWith('http://') || trimmed.startsWith('https://') || trimmed.startsWith('/')) {
      return trimmed
    }
  }

  return DEFAULT_PRODUCT_FALLBACK
}

/**
 * Resolves an authentic hero image for category cards using local verified assets.
 */
export function getCategoryImageUrl(categoryName?: string): string {
  if (!categoryName) return DEFAULT_PRODUCT_FALLBACK
  const name = categoryName.toLowerCase()

  if (
    name.includes('apricot') ||
    name.includes('appricot') ||
    name.includes('khubani') ||
    name.includes('halman')
  ) {
    return '/images/Apricot.jpg'
  }
  if (name.includes('walnut') || name.includes('akhrot') || name.includes('kagzi')) {
    return '/images/Walnut.jpg'
  }
  if (name.includes('almond') || name.includes('badam') || name.includes('giri')) {
    return '/images/Almond.jpg'
  }
  if (name.includes('oleaster') || name.includes('rassian olive') || name.includes('gold berry')) {
    return '/images/mulberries.jpg'
  }
  if (name.includes('date') || name.includes('dates') || name.includes('khajoor') || name.includes('khajur')) {
    return '/images/dates.jpg'
  }
  if (name.includes('raisins') || name.includes('kishmis') || name.includes('dried raisins')) {
    return '/images/raisins.jpg'
  }
  if (name.includes('pistachio') || name.includes('pistachios') || name.includes('pista')) {
    return '/images/pistachio.jpg'
  }
  if (name.includes('cashew') || name.includes('cashews') || name.includes('kaju')) {
    return '/images/cashews.jpg'
  }
  if (name.includes('figs') || name.includes('fig') || name.includes('injeer')) {
    return '/images/figs.jpg'
  }
  if (
    name.includes('mulberry') || name.includes('shehtoot') || name.includes('shahtoot') || name.includes('tut') ||
    name.includes('bidana')
  ) {
    return '/images/Bidana mulberries.jpg'
  }

  return DEFAULT_PRODUCT_FALLBACK
}

/**
 * Classifies whether a category name represents nuts or dry fruits.
 */
export function isNutCategory(categoryName?: string): boolean {
  if (!categoryName) return false
  const n = categoryName.toLowerCase()
  return (
    n.includes('nut') ||
    n.includes('walnut') ||
    n.includes('akhrot') ||
    n.includes('almond') ||
    n.includes('badam') ||
    n.includes('chilgoza') ||
    n.includes('pine') ||
    n.includes('pistachio') ||
    n.includes('pista') ||
    n.includes('cashew') ||
    n.includes('peanut') ||
    n.includes('hazelnut')
  )
}
