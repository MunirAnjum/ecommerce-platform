import { createRouter, createWebHistory } from 'vue-router'

import Home from '../pages/Home.vue'
import Login from '../pages/Login.vue'
import Products from '../pages/Products.vue'
import MainLayout from '../layouts/MainLayout.vue'
import ProductDetails from '../pages/ProductDetails.vue'
import Cart from '../pages/Cart.vue'
import Checkout from '../pages/Checkout.vue'
import MyOrders from '../pages/MyOrders.vue'
import OrderDetails from '../pages/OrderDetails.vue'
import AdminOrders from '../pages/AdminOrders.vue'

const router = createRouter({
  history: createWebHistory(),

  routes: [
    {
        path: '/',
        component: MainLayout,
        children:[
            {
                path: '/',
                name: 'home',
                component: Home,
            },

            {
                path: '/products',
                name: 'products',
                component: Products,
                meta: {
                    requiresAuth: true,
                },
            },

            {
                path: 'products/:id',
                name: 'product-details',
                component: ProductDetails,
                meta: {
                    requiresAuth: true,
                },
            },

            {
                path: '/cart',
                name: 'cart',
                component: Cart,
                meta:{
                    requiresAuth: true
                },
            },

            {
                path: '/checkout',
                name: 'checkout',
                component: Checkout,
                meta:{
                    requiresAuth: true
                }
            },

            {
                path: 'my-orders',
                name: 'my-orders',
                component: MyOrders,
                meta: { requiresAuth: true },
            },

            {
                path: 'my-orders/:id',
                name: 'order-details',
                component: OrderDetails,
                meta: { requiresAuth: true },
            },

            {
                path: '/admin/orders',
                name: 'admin-orders',
                component: AdminOrders,
                meta: {
                    requiresAuth: true,
                    requiresAdmin: true,
                },
            },
        ],
    },
    
    {
      path: '/login',
      name: 'login',
      component: Login,
    },
  ],
})

function hasAdminRole(token: string): boolean {
  try {
    const payload = token.split('.')[1]

    if (!payload) return false

    const normalized = payload
      .replace(/-/g, '+')
      .replace(/_/g, '/')

    const decoded = JSON.parse(atob(normalized)) as Record<string, unknown>

    const role =
      decoded.role ??
      decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']

    if (Array.isArray(role)) {
      return role.includes('Admin')
    }

    return role === 'Admin'
  } catch {
    return false
  }
}

router.beforeEach((to) => {
  const accessToken = localStorage.getItem('accessToken')

  if (to.meta.requiresAuth && !accessToken) {
    return { name: 'login' }
  }

  if (to.meta.requiresAdmin) {
    if (!accessToken) {
      return { name: 'login' }
    }

    if (!hasAdminRole(accessToken)) {
      return { name: 'home' }
    }
  }

  if (to.name === 'login' && accessToken) {
    return { name: 'home' }
  }
})

export default router