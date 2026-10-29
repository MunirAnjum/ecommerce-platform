import { createRouter, createWebHistory } from 'vue-router'

import MainLayout from '../layouts/MainLayout.vue'
import AdminLayout from '../layouts/AdminLayout.vue'

import Home from '../pages/Home.vue'
import Products from '../pages/Products.vue'
import ProductDetails from '../pages/ProductDetails.vue'
import Cart from '../pages/Cart.vue'
import Checkout from '../pages/Checkout.vue'
import MyOrders from '../pages/MyOrders.vue'
import OrderDetails from '../pages/OrderDetails.vue'
import Login from '../pages/Login.vue'
import Register from '../pages/Register.vue'

import AdminDashboard from '../pages/AdminDashboard.vue'
import AdminOrders from '../pages/AdminOrders.vue'
import AdminProducts from '../pages/AdminProducts.vue'
import AdminCategories from '../pages/AdminCategories.vue'
import AdminInventory from '../pages/AdminInventory.vue'
import CategoryLanding from '../pages/CategoryLanding.vue'

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

    return role === 'Admin' || role === 2 || role === '2'
  } catch {
    return false
  }
}

const router = createRouter({
  history: createWebHistory(),
  scrollBehavior(_to, _from, savedPosition) {
    if (savedPosition) {
      return savedPosition
    }
    return { top: 0 }
  },

  routes: [
    // Customer Storefront routes
    {
      path: '/',
      component: MainLayout,
      children: [
        {
          path: '',
          name: 'home',
          component: Home,
        },
        {
          path: 'products',
          name: 'products',
          component: Products,
        },
        {
          path: 'products/:id',
          name: 'product-details',
          component: ProductDetails,
        },
        {
          path: 'categories',
          name: 'categories',
          component: CategoryLanding,
          props: { categoryType: 'all' },
        },
        {
          path: 'dry-fruits',
          name: 'dry-fruits',
          component: CategoryLanding,
          props: { categoryType: 'dry-fruits' },
        },
        {
          path: 'nuts',
          name: 'nuts',
          component: CategoryLanding,
          props: { categoryType: 'nuts' },
        },
        {
          path: 'cart',
          name: 'cart',
          component: Cart,
          meta: { requiresAuth: true },
        },
        {
          path: 'checkout',
          name: 'checkout',
          component: Checkout,
          meta: { requiresAuth: true },
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
          path: 'login',
          name: 'login',
          component: Login,
        },
        {
          path: 'register',
          name: 'register',
          component: Register,
        },
      ],
    },

    // Admin Portal routes
    {
      path: '/admin',
      component: AdminLayout,
      meta: {
        requiresAuth: true,
        requiresAdmin: true,
      },
      children: [
        {
          path: '',
          name: 'admin-dashboard',
          component: AdminDashboard,
        },
        {
          path: 'orders',
          name: 'admin-orders',
          component: AdminOrders,
        },
        {
          path: 'products',
          name: 'admin-products',
          component: AdminProducts,
        },
        {
          path: 'categories',
          name: 'admin-categories',
          component: AdminCategories,
        },
        {
          path: 'inventory',
          name: 'admin-inventory',
          component: AdminInventory,
        },
      ],
    },

    // Catch all fallback
    {
      path: '/:pathMatch(.*)*',
      redirect: '/',
    },
  ],
})

router.beforeEach((to) => {
  const accessToken = localStorage.getItem('accessToken')
  const storedRole = localStorage.getItem('role')

  if (to.meta.requiresAuth && !accessToken) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  if (to.meta.requiresAdmin) {
    if (!accessToken) {
      return { name: 'login', query: { redirect: to.fullPath } }
    }

    const isAdmin = storedRole === '2' || hasAdminRole(accessToken)
    if (!isAdmin) {
      return { name: 'home' }
    }
  }

  if ((to.name === 'login' || to.name === 'register') && accessToken) {
    return { name: 'home' }
  }
})

export default router