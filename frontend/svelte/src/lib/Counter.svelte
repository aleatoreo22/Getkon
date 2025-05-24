<script lang="ts">
  let count: number = $state(0);
  const increment = async () => {
    count += 1;
  };
</script>

<button onclick={increment}>
  count is {count}
</button>
