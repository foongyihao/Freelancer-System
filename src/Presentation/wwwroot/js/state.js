import { api } from './api.js';
import { renderFreelancerCard, updatePager, clearError } from './ui.js';

// State management variables (exported for read-only external use if needed)
export let currentPage = 1;
export let totalPages = 1;
export const pageSize = 10;
export let currentSearch = '';
export let currentSkillFilter = '';
export let currentHobbyFilter = '';

// Helper mutation functions (avoid direct reassignment of ES module imported bindings in consumers)
export function gotoPage(p){ currentPage = p; }
export function decPage(){ if(currentPage>1) currentPage--; }
export function incPage(){ if(currentPage<totalPages) currentPage++; }
export function resetSearchFilters(){ currentSearch=''; currentSkillFilter=''; currentHobbyFilter=''; }
export function setTotalPages(t){ totalPages = t; }

/**
 * Load a page of freelancers from the API applying current search term and
 * archived filter, then render and update pagination UI.
 * @returns {Promise<void>}
 */
export async function fetchFreelancers({ term, skill, hobby } = {}) {
  // Update stored filters if provided; otherwise use current stored values
  if (term !== undefined) currentSearch = term.trim();
  if (skill !== undefined) currentSkillFilter = skill.trim();
  if (hobby !== undefined) currentHobbyFilter = hobby.trim();
  const includeArchived = document.getElementById('showArchived').checked;
  
  
  const q = new URLSearchParams();
  q.set('includeArchived', includeArchived);
  q.set('page', currentPage);
  q.set('pageSize', pageSize);
  if (currentSearch) q.set('term', currentSearch);
  if (currentSkillFilter) q.set('skill', currentSkillFilter);
  if (currentHobbyFilter) q.set('hobby', currentHobbyFilter);

  const data = await api(`/api/v1/freelancers?${q.toString()}`)
    .catch(()=>null);
  const items = data?.items || [];
  setTotalPages(data?.totalPages || 1);
  renderFreelancerCard(items);
  updatePager();
}

/**
 * Capture the current search input value, reset paging, and reload list.
 * Uses unified GET endpoint with `term` query parameter.
 * @returns {Promise<void>}
 */
export async function search() {
  currentPage = 1; // reset pagination on new search
  return fetchFreelancers({
    term: document.getElementById('searchTerm').value,
    skill: document.getElementById('searchSkill')?.value || '',
    hobby: document.getElementById('searchHobby')?.value || ''
  });
}

export function refreshList(){
  return fetchFreelancers(); // uses stored values
}

/**
 * Convert a comma separated list string into an array of trimmed non-empty values.
 * @param {string} v - Raw comma separated string (may be empty).
 * @returns {string[]} Array of trimmed tokens (empty array if none).
 */
export function toList(v){ return v.split(',').map(x=>x.trim()).filter(Boolean); }
