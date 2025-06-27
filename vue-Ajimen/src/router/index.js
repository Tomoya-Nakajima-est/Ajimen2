import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '../views/LoginView.vue'

// import ShiftHistory from '@/views/ShiftHistory.vue'


const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'Home',
      redirect: '/login' // 初期表示はログインページへ
    },
    {
      path: '/login',
      name: 'login',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: LoginView,
    },
    {
      path: '/employee',
      name: 'employee',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import('../views/EmployeeView.vue'),
    },
    {
      path: '/parttime',
      name: 'parttime',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import('../views/PartTimeView.vue'),
    },
    {
      path: '/employeeshift',
      name: 'employeeshift',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import('../views/EmployeeShiftView.vue'),
    },
    {
      path: '/parttimeshift',
      name: 'parttimeshift',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import('../views/PartTimeShiftView.vue'),
    },
    {
      path: '/employeeorder',
      name: 'employeeorder',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import('../views/InventoryView.vue'),
    },
    // {
    //   path: '/employeeshifthistory',
    //   name: 'employeeshifthistory',
    //   component: () => import('../views/ShiftHistoryStaff.vue'),
    // },
    // {
    //   path: '/parttimeshifthistory',
    //   name: 'parttimeshifthistory',
    //   component: () => import('../views/ShiftHistoryPartTime.vue'),
    // },
  ],
})

export default router
