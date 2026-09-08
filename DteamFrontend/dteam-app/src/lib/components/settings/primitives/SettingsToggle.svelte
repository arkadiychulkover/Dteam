<script lang="ts">
  interface Props {
    checked: boolean;
    disabled?: boolean;
    ariaLabel?: string;
    onchange?: (checked: boolean) => void;
  }
  let { checked = $bindable(false), disabled = false, ariaLabel = '', onchange }: Props = $props();

  function toggle() {
    if (disabled) return;
    checked = !checked;
    onchange?.(checked);
  }
</script>

<button
  type="button"
  role="switch"
  aria-checked={checked}
  aria-label={ariaLabel}
  {disabled}
  onclick={toggle}
  class="w-11 h-6 flex items-center rounded-full p-1 transition-colors duration-200 cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed focus:outline-none focus:ring-2 focus:ring-cyan-400/40 {checked ? 'bg-[#ff5b79]' : 'bg-[#031d27] border border-cyan-900/50'}"
>
  <div
    class="w-4 h-4 rounded-full bg-white shadow-md transform transition-transform duration-200 {checked ? 'translate-x-5' : 'translate-x-0'}"
  ></div>
</button>
