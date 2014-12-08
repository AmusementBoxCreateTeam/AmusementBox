<?php $this->load->view('common/header'); ?>
<div class="container">
    <div id="top_content" class="text-center">
        <div class="top_contents">
            <h1>ユーザ</h1>
            <ul>
                <li>
                    <a href="<?php echo base_url() ?>index.php/user">一覧</a>
                </li>
            </ul>
        </div>
        <div class="top_contents">
            <h1>BOX</h1>
            <ul>
                <li>
                    <a href="<?php echo base_url() ?>index.php/box">一覧</a>
                </li>
                <li>
                    <a href="<?php echo base_url() ?>index.php/box/register">登録</a>
                </li>
            </ul>
        </div>
        <div class="top_contents">
            <h1>音楽</h1>
            <ul>
                <li>
                    <a href="<?php echo base_url() ?>index.php/music">一覧</a>
                </li>
                <li>
                    <a href="<?php echo base_url() ?>index.php/music/register">登録</a>
                </li>
                <li>
                    <a href="<?php echo base_url() ?>index.php/statistic">ランキング</a>
                </li>
            </ul>
        </div>
    </div>
</div>
<?php $this->load->view('common/footer'); ?>
