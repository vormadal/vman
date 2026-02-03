/**
 * Example: How to read items from the API
 * 
 * The Kiota client is generated in src/lib/api/generated/
 * You need to wrap it with authentication - see kiota-client.ts
 */

// Example usage with the generated Kiota client:
// 
// import { createApiClient } from '@/lib/api/kiota-client'
// 
// const client = createApiClient()
// 
// // Get all items
// const items = await client.api.items.get()
//
// // Get untagged videos
// const untaggedVideos = await client.api.items.get({
//   queryParameters: {
//     untagged: true,
//     type: 'video'
//   }
// })
//
// // Get with pagination
// const page2 = await client.api.items.get({
//   queryParameters: {
//     page: 2,
//     pageSize: 20
//   }
// })

export const ITEMS_API_EXAMPLES = {
  getAllItems: 'client.api.items.get()',
  getUntaggedVideos: 'client.api.items.get({ queryParameters: { untagged: true, type: "video" } })',
  getFavorites: 'client.api.items.get({ queryParameters: { isFavorite: true } })',
  paginate: 'client.api.items.get({ queryParameters: { page: 2, pageSize: 20 } })'
}
