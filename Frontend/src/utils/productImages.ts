/**
 * Product & Category image resolver with authentic Baltistan dry fruit photography,
 * persistent local storage image registry, and graceful fallback handling.
 */

// Curated high-resolution imagery for real dry fruits
const FRUIT_IMAGE_MAP: Record<string, string> = {
  // Apricot & derivatives
  apricot: 'https://images.unsplash.com/photo-1595231712325-9fcca684346e?auto=format&fit=crop&w=800&q=80',
  appricot: 'https://images.unsplash.com/photo-1595231712325-9fcca684346e?auto=format&fit=crop&w=800&q=80',
  khubani: 'https://images.unsplash.com/photo-1595231712325-9fcca684346e?auto=format&fit=crop&w=800&q=80',
  halman: 'https://images.unsplash.com/photo-1595231712325-9fcca684346e?auto=format&fit=crop&w=800&q=80',

  // Walnuts
  walnut: 'https://images.unsplash.com/photo-1563227812-0ea4c22e6cc8?auto=format&fit=crop&w=800&q=80',
  akhrot: 'https://images.unsplash.com/photo-1563227812-0ea4c22e6cc8?auto=format&fit=crop&w=800&q=80',
  kagzi: 'https://images.unsplash.com/photo-1563227812-0ea4c22e6cc8?auto=format&fit=crop&w=800&q=80',

  // Almonds
  almond: 'https://images.unsplash.com/photo-1508061253366-f7da158b6d46?auto=format&fit=crop&w=800&q=80',
  badam: 'https://images.unsplash.com/photo-1508061253366-f7da158b6d46?auto=format&fit=crop&w=800&q=80',
  giri: 'https://images.unsplash.com/photo-1508061253366-f7da158b6d46?auto=format&fit=crop&w=800&q=80',

  // Pine nuts / Chilgoza
  pine: 'https://images.unsplash.com/photo-1577968897966-3d4325b36b61?auto=format&fit=crop&w=800&q=80',
  chilgoza: 'https://images.unsplash.com/photo-1577968897966-3d4325b36b61?auto=format&fit=crop&w=800&q=80',

  // Mulberry
  mulberry: 'https://images.unsplash.com/photo-1596797882870-8c33deeac224?auto=format&fit=crop&w=800&q=80',
  shehtoot: 'https://images.unsplash.com/photo-1596797882870-8c33deeac224?auto=format&fit=crop&w=800&q=80',
  shahtoot: 'https://images.unsplash.com/photo-1596797882870-8c33deeac224?auto=format&fit=crop&w=800&q=80',
  tut: 'https://images.unsplash.com/photo-1596797882870-8c33deeac224?auto=format&fit=crop&w=800&q=80',

  // Figs
  fig: 'https://images.unsplash.com/photo-1601493700631-2b16ec4b4716?auto=format&fit=crop&w=800&q=80',
  injeer: 'https://images.unsplash.com/photo-1601493700631-2b16ec4b4716?auto=format&fit=crop&w=800&q=80',

  // Raisins
  raisin: 'https://images.unsplash.com/photo-1589135233689-d56b4f74eeeb?auto=format&fit=crop&w=800&q=80',
  kishmish: 'https://images.unsplash.com/photo-1589135233689-d56b4f74eeeb?auto=format&fit=crop&w=800&q=80',
  sundarkhani: 'https://images.unsplash.com/photo-1589135233689-d56b4f74eeeb?auto=format&fit=crop&w=800&q=80',

  // Pistachios
  pistachio: 'https://images.unsplash.com/photo-1615485290382-441e4d049cb5?auto=format&fit=crop&w=800&q=80',
  pista: 'https://images.unsplash.com/photo-1615485290382-441e4d049cb5?auto=format&fit=crop&w=800&q=80',

  // Mixed / Default
  mix: 'https://images.unsplash.com/photo-1599599810769-bcde5a160d32?auto=format&fit=crop&w=800&q=80',
  dry: 'https://images.unsplash.com/photo-1599599810769-bcde5a160d32?auto=format&fit=crop&w=800&q=80',
}

export const DEFAULT_PRODUCT_FALLBACK =
  'https://images.unsplash.com/photo-1599599810769-bcde5a160d32?auto=format&fit=crop&w=800&q=80'

const PRODUCT_IMAGES_STORAGE_KEY = 'karakoram_product_custom_images'

/**
 * Returns dictionary of custom product images persisted in localStorage.
 */
export function getCustomImagesRegistry(): Record<string, string> {
  try {
    const raw = localStorage.getItem(PRODUCT_IMAGES_STORAGE_KEY)
    if (raw) {
      return JSON.parse(raw)
    }
  } catch (err) {
    console.error('Failed to parse product custom images from localStorage:', err)
  }
  return {}
}

/**
 * Persists a custom image (DataURL or URL) for a product ID in browser storage.
 */
export function saveProductCustomImage(productId: string, imageDataUrlOrUrl: string): void {
  try {
    const registry = getCustomImagesRegistry()
    registry[productId] = imageDataUrlOrUrl
    localStorage.setItem(PRODUCT_IMAGES_STORAGE_KEY, JSON.stringify(registry))
    window.dispatchEvent(new CustomEvent('product-images-updated', { detail: { productId } }))
  } catch (err) {
    console.error('Failed to save product custom image:', err)
  }
}

/**
 * Removes custom persisted image for a product.
 */
export function removeProductCustomImage(productId: string): void {
  try {
    const registry = getCustomImagesRegistry()
    if (registry[productId]) {
      delete registry[productId]
      localStorage.setItem(PRODUCT_IMAGES_STORAGE_KEY, JSON.stringify(registry))
      window.dispatchEvent(new CustomEvent('product-images-updated', { detail: { productId } }))
    }
  } catch (err) {
    console.error('Failed to remove product custom image:', err)
  }
}

/**
 * Retrieves the custom image for a given product ID if one exists.
 */
export function getProductCustomImage(productId?: string): string | null {
  if (!productId) return null
  const registry = getCustomImagesRegistry()
  return registry[productId] || null
}

/**
 * Resolves the appropriate product image URL based on custom uploaded image,
 * backend explicit URL, or authentic Baltistan photography fallback.
 */
export function getProductImageUrl(
  name?: string,
  categoryName?: string,
  explicitUrl?: string,
  productId?: string
): string {
  // 1. Explicit URL from caller or backend
  if (explicitUrl && explicitUrl.trim().length > 0) {
    return explicitUrl
  }

  // 2. Custom image persisted locally by product ID
  if (productId) {
    const custom = getProductCustomImage(productId)
    if (custom) return custom
  }

  // 3. Match against authentic photo keywords
  const searchTarget = `${name ?? ''} ${categoryName ?? ''}`.toLowerCase()

  for (const [key, url] of Object.entries(FRUIT_IMAGE_MAP)) {
    if (searchTarget.includes(key)) {
      return url
    }
  }

  return DEFAULT_PRODUCT_FALLBACK
}

/**
 * Resolves an authentic hero image for category cards.
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
    return FRUIT_IMAGE_MAP.apricot
  }
  if (name.includes('walnut') || name.includes('akhrot') || name.includes('kagzi')) {
    return FRUIT_IMAGE_MAP.walnut
  }
  if (name.includes('almond') || name.includes('badam') || name.includes('giri')) {
    return FRUIT_IMAGE_MAP.almond
  }
  if (name.includes('pine') || name.includes('chilgoza')) {
    return FRUIT_IMAGE_MAP.chilgoza
  }
  if (
    name.includes('mulberry') ||
    name.includes('shehtoot') ||
    name.includes('shahtoot') ||
    name.includes('tut')
  ) {
    return FRUIT_IMAGE_MAP.mulberry
  }
  if (name.includes('fig') || name.includes('injeer')) {
    return FRUIT_IMAGE_MAP.fig
  }
  if (name.includes('raisin') || name.includes('kishmish')) {
    return FRUIT_IMAGE_MAP.raisin
  }
  if (name.includes('pistachio') || name.includes('pista')) {
    return FRUIT_IMAGE_MAP.pistachio
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
